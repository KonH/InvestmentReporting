<template>
	<table class="table table-sm table-striped">
		<thead>
			<tr>
				<th scope="col" style="width: 10%">ISIN</th>
				<th scope="col" style="width: 25%">Name</th>
				<th scope="col" style="width: 65%">Tags</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="asset in assets" :key="asset.isin">
				<tag-asset :asset="asset" />
			</tr>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action, State } from 'vuex-class';
import { AssetTagStateDto } from '@/api/meta';
import TagAsset from '@/component/tag/tagAsset.vue';

@Options({
	name: 'TagsView',
	components: {
		TagAsset,
	},
})
export default class TagsView extends Vue {
	@State('tagState')
	tagState!: AssetTagStateDto;

	@Action('fetchTagState')
	fetchTagState!: () => void;

	get assets() {
		return this.tagState.assets;
	}
}
</script>
