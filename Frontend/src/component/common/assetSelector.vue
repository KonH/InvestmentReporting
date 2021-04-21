<template>
	<label>
		Asset:
		<select ref="asset" class="form-control" @input="onInput">
			<option v-for="asset in assets" :key="getAssetKey(asset)" :value="getAssetKey(asset)">
				{{ renderAsset(asset) }}
			</option>
		</select>
	</label>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action, State } from 'vuex-class';
import { AssetDto, StateDto } from '@/api/state';
import { Prop, Ref } from 'vue-property-decorator';
import { VirtualStateDto } from '@/api/market';

@Options({
	name: 'AssetSelector',
	emits: ['input'],
})
export default class AssetSelector extends Vue {
	@Prop()
	brokerId: string | undefined;

	@Prop()
	value!: string;

	@Prop()
	emitIsin!: boolean;

	@State('activeState')
	activeState!: StateDto;

	@State('virtualState')
	virtualState!: VirtualStateDto;

	@Ref('asset')
	assetSelect!: HTMLSelectElement;

	@Action('fetchState')
	fetchState!: () => void;

	get assets() {
		const brokers = this.activeState.brokers?.filter((b) => !this.brokerId || b.id == this.brokerId);
		const viewArray: [AssetDto] = [{ id: '' }];
		const inventory = brokers?.flatMap((b) => b.inventory ?? []) ?? [];
		return viewArray.concat(inventory);
	}

	getAssetName(id: string) {
		const assets = this.virtualState.balances?.map((b) => b.inventory?.find((a) => a.id == id))?.filter((a) => a);
		return assets && assets.length > 0 ? assets[0]?.name : undefined;
	}

	getAssetKey(asset: AssetDto | null) {
		return this.emitIsin ? asset?.isin : asset?.id;
	}

	renderAsset(asset: AssetDto | null) {
		if (asset?.id) {
			const name = this.getAssetName(asset.id);
			return name ? name : `${asset.isin}`;
		}
		return '';
	}

	onInput() {
		this.$emit('input', this.assetSelect.value);
	}
}
</script>
