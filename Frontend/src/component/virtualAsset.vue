<template>
	{{ asset.isin }}/{{ asset.type }} <b>{{ asset.name }} x{{ asset.count }}</b
	>&#160; <money :value="asset.virtualPrice" :currency-id="asset.currency" />&#160;
	<money-diff :old="asset.realPrice" :new="asset.virtualPrice" :currency-id="asset.currency" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { VirtualAssetDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';

@Options({
	name: 'VirtualAsset',
	components: {
		Money,
		MoneyDiff,
	},
})
export default class VirtualAsset extends Vue {
	@Prop()
	asset!: VirtualAssetDto;
}
</script>
