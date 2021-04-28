<template>
	<span>
		<span>{{ tag }}</span>
		<button :class="buttonClass" @click="remove">-</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { AssetTagSetDto } from '@/api/meta';
import { Action } from 'vuex-class';
import Backend from '@/service/backend';
import Progress from '@/utils/progress';

@Options({
	name: 'TagCard',
})
export default class TagCard extends Vue {
	@Action('fetchTagState')
	fetchTagState!: () => void;

	@Prop()
	asset!: AssetTagSetDto;

	@Prop()
	tag!: string;

	isInProgress = false;

	get buttonClass() {
		return Progress.getClass(this, 'ml-1 btn btn-sm btn-outline-danger');
	}

	async remove() {
		await Progress.wrap(this, this.removeApply);
	}

	async removeApply() {
		await Backend.meta().tag.removeCreate({
			asset: this.asset.isin,
			tag: this.tag,
		});
		this.fetchTagState();
	}
}
</script>
